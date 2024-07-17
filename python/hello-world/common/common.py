from typing import Dict
import json


class NameIdDict:
    def __init__(self, data: Dict[str, str]):
        """
        Initializes a NameIdDict instance.

        This class serves as a container for a dictionary where the keys are result names
        and the values are their associated IDs.

        Args:
            data: Dictionary with result names as keys and their associated IDs as values.
        """
        self.data = data

    def serialize(self) -> bytes:
        """
        Serializes the dictionary to a JSON-encoded byte array.

        This method converts the dictionary to a JSON string and then encodes it to bytes.

        Returns:
            bytes: The serialized dictionary as a byte array.
        """
        return json.dumps(self.data).encode("utf-8")

    @classmethod
    def deserialize(cls, payload: bytes) -> "NameIdDict":
        """
        Deserializes bytes into a NameIdDict instance.

        This method decodes the byte array to a JSON string and then loads it into a dictionary
        to create a NameIdDict instance.

        Args:
            payload (bytes): The serialized data as bytes.

        Returns:
            NameIdDict: An instance of NameIdDict created from the serialized data.
        """
        return cls(json.loads(payload.decode("utf-8")))
