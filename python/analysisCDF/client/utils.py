import logging
import time
from typing import Any, Callable

# Configure logging
logging.basicConfig(
    level=logging.INFO, format="%(asctime)s - %(levelname)s - %(message)s"
)
logger = logging.getLogger(__name__)

# Constants
MAX_ATTEMPTS = 3
BATCH_SIZE_FOR_RESULTS = 1000


def retry_operation(
    operation: Callable, max_attempts=MAX_ATTEMPTS, operation_name="Operation"
) -> Any:
    """Retry an operation with exponential backoff."""
    for attempt in range(1, max_attempts + 1):
        try:
            result = operation()
            return result
        except Exception as e:
            logger.error("%s attempt %d failed: %s", operation_name, attempt, e)
            if attempt == max_attempts:
                raise ValueError(f"Max retries reached for {operation_name}") from e
            sleep_time = 2**attempt
            logger.info("Retrying in %d seconds...", sleep_time)
            time.sleep(sleep_time)
