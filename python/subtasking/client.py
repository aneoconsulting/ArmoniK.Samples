import argparse
from datetime import timedelta

import grpc
from armonik.client import ArmoniKTasks, ArmoniKResults, ArmoniKSessions, ArmoniKEvents
from armonik.common import TaskOptions, TaskDefinition


def parse_arguments():
    """
    Parse command line arguments
    Returns:
    Parsed arguments
    """
    parser = argparse.ArgumentParser(
        description="ArmoniK Example Client",
        epilog="This example computes the sum of the N first integers\n Example : \n python client.py 20",
    )

    connection_args = parser.add_argument_group(
        title="Connection", description="Connection arguments"
    )
    connection_args.add_argument(
        "-e",
        "--endpoint",
        default="localhost:5001",
        help="ArmoniK control plane endpoint",
        type=str,
        required=True,
    )
    connection_args.add_argument(
        "--ssl",
        help="Use this option to enable TLS for a secure channel.",
        action="store_true",
    )
    connection_args.add_argument(
        "--ca",
        help="ca.crt path to the certificate authority for TLS or mutual TLS, if not installed",
        type=str,
    )
    connection_args.add_argument(
        "--crt", help="client certificate path for mutual TLS", type=str
    )
    connection_args.add_argument(
        "--key", help="client key path for mutual TLS", type=str
    )

    armonik_args = parser.add_argument_group(
        title="ArmoniK", description="ArmoniK arguments"
    )
    armonik_args.add_argument("-p", "--partition", help="Partition to use", type=str)

    payload_args = parser.add_argument_group(
        title="Payload", description="Payload arguments"
    )
    payload_args.add_argument(
        "-s",
        "--split",
        help="Threshold of number of values where the task will be split",
        type=int,
        default=10,
    )
    payload_args.add_argument("N", help="Number of values to sum", type=int)

    return parser.parse_args()


def read_file(file_path: str) -> bytes:
    """
    Reads binary file
    Args:
        file_path: File path to read

    Returns:
        File content
    """
    with open(file_path, "rb") as file:
        return file.read()


def create_channel(
    endpoint: str, ssl: bool, ca: str, key: str, cert: str
) -> grpc.Channel:
    """
    Create a gRPC channel for communication with the ArmoniK control plane

    Args:
        ssl:
        ca (str): CA file path for TLS or mutual TLS
        cert (str): Certificate file path for mutual TLS
        key (str): Private key file path for mutual TLS
        endpoint (str): ArmoniK control plane endpoint

    Returns:
        grpc.Channel: gRPC channel for communication
    """
    if ssl:
        ca_data = read_file(ca) if ca else None
        if cert and key:
            cert_data = read_file(cert) if cert else None
            key_data = read_file(key) if key else None
            credentials = grpc.ssl_channel_credentials(
                root_certificates=ca_data,
                private_key=key_data,
                certificate_chain=cert_data,
            )
            print("Hello ArmoniK Python Example Using Mutual TLS!")
        else:
            credentials = grpc.ssl_channel_credentials(root_certificates=ca_data)
            print("Hello ArmoniK Python Example Using TLS!")
        return grpc.secure_channel(endpoint, credentials)
    else:
        print("Hello ArmoniK Python Example using Insecure Channel!")
        return grpc.insecure_channel(endpoint)


def main():
    args = parse_arguments()
    # Open a channel to the control plane
    with create_channel(
        args.endpoint, args.ssl, args.ca, args.key, args.crt
    ) as channel:
        # Create a task submitting client
        tasks_client = ArmoniKTasks(channel)
        # Create the results client
        results_client = ArmoniKResults(channel)
        # Create the session client
        session_client = ArmoniKSessions(channel)
        # Default task options to be used in a session
        default_task_options = TaskOptions(
            max_duration=timedelta(seconds=300),
            priority=1,
            max_retries=5,
            partition_id=args.partition,
            options={"split": str(args.split)},
        )
        # Create a session
        session_id = session_client.create_session(
            default_task_options=default_task_options,
            partition_ids=[args.partition] if args.partition is not None else None,
        )
        print(f"Session {session_id} has been created")

        # Create payload and result
        results_created = results_client.create_results_metadata(
            ["payload", "result"], session_id
        )
        payload_id = results_created["payload"].result_id
        result_id = results_created["result"].result_id

        # Create payload data
        payload = bytearray()
        for i in range(1, args.N + 1):
            payload.extend(i.to_bytes(4, "little"))
        payload = bytes(payload)
        # Send payload
        results_client.upload_result_data(payload_id, session_id, payload)

        # Create task definition
        task_definition = TaskDefinition(
            payload_id=payload_id, expected_output_ids=[result_id]
        )

        # Submit task
        tasks_client.submit_tasks(session_id, [task_definition])

        # Wait for result availability
        event_client = ArmoniKEvents(channel)
        event_client.wait_for_result_availability(result_id, session_id)

        # Download the result
        result_data = results_client.download_result_data(result_id, session_id)

        # Convert it to int
        result = int.from_bytes(result_data, "little")

        # Verify
        expected = args.N * (args.N + 1) // 2
        print(f"Result: {result}, Expected: {expected}")

        # Done close the session
        session_client.close_session(session_id)

        # Cleanup
        session_client.purge_session(session_id)
        session_client.delete_session(session_id)


if __name__ == "__main__":
    main()
