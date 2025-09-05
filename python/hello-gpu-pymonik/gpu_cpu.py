from pymonik import Pymonik, Task, task
import jax
import timeit
import numpy as np

# Just to differenciate the GPU session and the CPU session.
@task
def ref_devices():
    return str(jax.devices())

# Here we apply a simple activation function to a basic layer.
@jax.jit
def layer(x, weights, bias):
    output = []
    for i in range(x.shape[0]):
        output.append(jax.nn.sigmoid(x[i] @ weights + bias))
    return (output)

# We take the jit-ed function to create a AK task.
sigmo_task = Task(layer)

if __name__ == "__main__":
    # Create the input
    rng = np.random.default_rng()
    x = rng.normal(size=(10, 250))
    weights = rng.normal(size=(250, 40))
    bias = rng.normal(size=(40,))

    # Create two versions of Pymonyk with its own partition and the right JAX version.
    pymonikgpu = Pymonik(partition="gpumonik", environment={"pip":["jax[cuda12]",]})
    pymonikcpu = Pymonik(partition="pymonik", environment={"pip":["jax",]})

    with pymonikgpu:
        device = ref_devices.invoke().wait().get()
        print(device)
        result_gpu = sigmo_task.invoke(x, weights, bias).wait().get()
        print("Layer = " + str(result_gpu))

    with pymonikcpu:
        device = ref_devices.invoke().wait().get()
        print(device)
        result_cpu = sigmo_task.invoke(x, weights, bias).wait().get()
        print("Layer = " + str(result_cpu))
    