# Range Sum Library - Subtasking from a Dynamically Loaded Library

This sample shows a library, loaded at runtime by the Java dynamic worker, that submits its own subtasks.

The client ([`RangeSum`](../../client/src/main/java/fr/aneo/armonik/client/samples/RangeSum.java)) submits a single
task computing

$$S(lo, hi) = \sum_{i=lo}^{hi-1} i^2$$

for $lo = 1$, $hi = N + 1$. `RangeSumProcessor` evaluates it by recursive splitting:

- If $hi - lo \le grain$, the task computes the sum directly and writes its `result` output.
- Otherwise it picks $mid = lo + \lfloor (hi - lo)/2 \rfloor$ and submits three subtasks:
  - `left` on $[lo, mid)$,
  - `right` on $[mid, hi)$,
  - `combine`, with inputs the outputs of `left` and `right`, and output the parent's own `result`.

The parent returns without writing `result`: ownership of that output passes to the `combine` subtask. ArmoniK starts
each `combine` task only once both of its inputs exist. With $N = 1000$ and $grain = 100$ the tree has 16 leaves,
15 split tasks and 15 combine tasks, and the result is $N(N+1)(2N+1)/6 = 333\,833\,500$.

## Worker library of the subtasks

A task processed by the dynamic worker carries the library to load in its options (`LibraryBlobId`, `LibraryPath`,
`Symbol`, `ConventionVersion`), and the library blob in its data dependencies. Subtasks need the same, otherwise the
dynamic worker cannot load any code for them. The processor passes its own library along:

```java
var library = taskContext.workerLibrary();
taskContext.submitTask(new TaskDefinition().withInput("lo", taskContext.getInput("lo"))
                                           .withInput("hi", midBlob)
                                           .withInput("grain", taskContext.getInput("grain"))
                                           .withOutput("result")
                                           .withWorkerLibrary(library));
```

When no configuration is set on the subtask definition, the subtask reuses the configuration of the running task
(partition, priority, retries) with the library options set. `library.withSymbol("...")` targets another class from
the same archive.

## Requirements

`TaskContext#workerLibrary()` and the worker-side `TaskDefinition#withWorkerLibrary(...)` are not in a released
ArmoniK.Extensions.Java version yet. Both the library and the `javadynamic` worker image must be built from a version
that has them:

```bash
# In ArmoniK.Extensions.Java/worker
./mvnw -Pdocker -DskipTests -Djib.skip=true install
./mvnw -Pdocker -pl armonik-worker jib:dockerBuild \
  -Djib.to.image=armonik-dynamic-java-worker -Djib.to.tags=subtask-local -Djib.from.platforms=linux/amd64
```

The `install` step puts `armonik-worker-domain:1.0.0-SNAPSHOT` in the local Maven repository for this library, and
`jib:dockerBuild` builds the worker image into the local Docker daemon. Use that image for the `javadynamic` partition.

The library declares `armonik-worker-domain` with `provided` scope: the dynamic worker loads library classes with its
own class loader as parent, so the SDK classes always come from the worker.

## Building

```bash
mvn package
```

This produces `target/range-sum-library.zip`, containing `range-sum-library-1.0.0-SNAPSHOT.jar` at its root.

## Running

See [RangeSum](../../client/README.md#rangesum) in the client samples.
