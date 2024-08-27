from typing import Dict
import json
import numpy as np



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


class NumpyArraySerializer:
    def __init__(self, array: np.ndarray):
        """
        Initializes a NumpyArraySerializer instance.
        This class serves as a container for a numpy array of type float32.
        Args:
            array: numpy array to be serialized and deserialized.
        """
        self.array = array

    def serialize(self) -> bytes:
        """
        Serializes the numpy array to a JSON-encoded byte array.
        This method converts the numpy array to a list, encodes it as JSON,
        and then encodes it to bytes.
        Returns:
            bytes: The serialized numpy array as a byte array.
        """
        return self.array.tobytes()

    @classmethod
    def deserialize(cls, payload: bytes) -> "NumpyArraySerializer":
        """
        Deserializes bytes into a NumpyArraySerializer instance.
        This method decodes the byte array to a JSON string,
        loads it into a list, and converts it to a numpy array.
        Args:
            payload (bytes): The serialized data as bytes.
        Returns:
            NumpyArraySerializer: An instance of NumpyArraySerializer created from the serialized data.
        """
        array = np.frombuffer(payload, dtype=np.float32)
        return cls(array)