from pymonik import Pymonik, task
import jax

# Use this task to check if your GPU is correctly detected by Kubernetes inside your docker container.

@task
def ref_devices():
    return str(jax.devices())

if __name__ == "__main__":
    pymonik = Pymonik(partition="gpumonik", environment={"pip":["jax[cuda12]"]})
    with pymonik:
        D = ref_devices.invoke().wait().get()
        print(D)