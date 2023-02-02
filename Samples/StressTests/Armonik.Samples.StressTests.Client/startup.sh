#!/bin/bash


mkdir -p /tmp/stresstest/output/
echo "running stress tests with config Components__ObjectStorage = $Components__ObjectStorage"
echo "$Components__ObjectStorage" > /tmp/stresstest/output/ObjectStorage.txt


declare -a stressTestParameters=(
"--nbTask 224  --nbInputBytes 4029815 --nbOutputBytes 226016  --workLoadTimeInMs 46560"
"--nbTask 2688 --nbInputBytes 4132360 --nbOutputBytes 226204  --workLoadTimeInMs 54100"
"--nbTask 224  --nbInputBytes 4029815 --nbOutputBytes 226016  --workLoadTimeInMs 46560"
"--nbTask 7842 --nbInputBytes 3116095 --nbOutputBytes 124792  --workLoadTimeInMs 60880"
"--nbTask 224  --nbInputBytes 4033628 --nbOutputBytes 226016  --workLoadTimeInMs 46790"
"--nbTask 8470 --nbInputBytes 3133743 --nbOutputBytes 124377  --workLoadTimeInMs 68960"
"--nbTask 222  --nbInputBytes 4058994 --nbOutputBytes 225432  --workLoadTimeInMs 47910"
"--nbTask 1206 --nbInputBytes 3123985 --nbOutputBytes 123774  --workLoadTimeInMs 51580"
"--nbTask 6609 --nbInputBytes 551060  --nbOutputBytes  25940  --workLoadTimeInMs 46110"
)

testIndex=0
date
for stressTestParameter in "${stressTestParameters[@]}"; do
let testIndex++
echo "Running stress test $testIndex : $stressTestParameter"
export outputfile=$(echo "${stressTestParameter// /_}.txt")
./Armonik.Samples.StressTests.Client stressTest $stressTestParameter > /tmp/stresstest/output/$outputfile
done

echo "stress tests are finished !!!"
date
echo "outputs are in /tmp/stresstest/output/ folder : "
ls -ail /tmp/stresstest/output/

# keep pod up 2 hours after end of the stress test, so you have time to retrieve files with 'kubectl cp...' nevertheless if pod is terminated you can still use the cp from docker
sleep 7200
