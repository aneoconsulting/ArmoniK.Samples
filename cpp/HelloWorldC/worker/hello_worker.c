#include <armonik/sdk/worker/ArmoniKSDKInterface.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

typedef struct {
  char name[256];
} service_ctx_t;

void *armonik_create_service(const char *service_namespace, const char *service_name) {
  service_ctx_t *ctx = (service_ctx_t *)malloc(sizeof(service_ctx_t));
  if (!ctx)
    return NULL;
  snprintf(ctx->name, sizeof(ctx->name), "%s::%s", service_namespace, service_name);
  printf("Creating service <%s>\n", ctx->name);
  return ctx;
}

void armonik_destroy_service(void *service_context) {
  service_ctx_t *ctx = (service_ctx_t *)service_context;
  printf("Destroying service <%s>\n", ctx->name);
  free(ctx);
}

void *armonik_enter_session(void *service_context, const char *session_id) {
  (void)service_context;
  char *session = strdup(session_id);
  printf("Entering session: %s\n", session);
  return session;
}

void armonik_leave_session(void *service_context, void *session_context) {
  (void)service_context;
  printf("Leaving session: %s\n", (char *)session_context);
  free(session_context);
}

armonik_status_t armonik_call(void *armonik_context, void *service_context, void *session_context,
                               const char *function_name, const char *input, size_t input_size,
                               armonik_callback_t callback) {
  (void)service_context;
  printf("Calling '%s' in session '%s' with %zu bytes\n", function_name, (char *)session_context, input_size);

  if (strcmp(function_name, "hello") == 0) {
    static const char suffix[] = " World!";
    const size_t suffix_len = sizeof(suffix) - 1;
    const size_t output_size = input_size + suffix_len;
    char *output = (char *)malloc(output_size);
    if (!output) {
      static const char err[] = "Out of memory";
      callback(armonik_context, ARMONIK_STATUS_ERROR, err, sizeof(err) - 1);
      return ARMONIK_STATUS_ERROR;
    }
    memcpy(output, input, input_size);
    memcpy(output + input_size, suffix, suffix_len);
    callback(armonik_context, ARMONIK_STATUS_OK, output, output_size);
    free(output);
    return ARMONIK_STATUS_OK;
  }

  if (strcmp(function_name, "error") == 0) {
    static const char err[] = "Permanent error requested by client";
    callback(armonik_context, ARMONIK_STATUS_ERROR, err, sizeof(err) - 1);
    return ARMONIK_STATUS_ERROR;
  }

  if (strcmp(function_name, "retry") == 0) {
    static const char msg[] = "Transient error requested by client, will retry";
    callback(armonik_context, ARMONIK_STATUS_RETRY, msg, sizeof(msg) - 1);
    return ARMONIK_STATUS_RETRY;
  }

  /* Unknown function */
  static const char err[] = "Unknown function";
  callback(armonik_context, ARMONIK_STATUS_ERROR, err, sizeof(err) - 1);
  return ARMONIK_STATUS_ERROR;
}
