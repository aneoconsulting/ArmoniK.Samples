package fr.aneo.armonik.client.samples.listener;

import fr.aneo.armonik.client.BlobCompletionListener;

import static java.nio.charset.StandardCharsets.UTF_8;

/**
 * Simple listener that prints blob completion events to stdout.
 */
public class SimpleBlobListener implements BlobCompletionListener {

  @Override
  public void onSuccess(Blob blob) {
    System.out.println("Blob completed - id: " + blob.blobInfo().id().asString() +
                       ", data: " + new String(blob.data(), UTF_8));
  }

  @Override
  public void onError(BlobError blobError) {
    System.out.println("Blob error - id: " + blobError.blobInfo().id().asString() +
                       ", message: " + blobError.cause().getMessage());
  }
}
