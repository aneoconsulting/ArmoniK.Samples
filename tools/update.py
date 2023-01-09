import subprocess
import argparse

parser = argparse.ArgumentParser(
    description="Set the version of the ArmoniK.Dev* packages to the new version",
    formatter_class=argparse.ArgumentDefaultsHelpFormatter,
)
parser.add_argument("new_version", help="New version", type=str)
args = parser.parse_args()

cmd = subprocess.Popen('git grep -l "" | grep "csproj$" | xargs grep -i armonik.de', shell=True, stdout=subprocess.PIPE, text=True)
cmd.wait()

command_res= cmd.communicate()[0].rstrip("\n").split("\n")

for line in command_res:
    file = line.split(":")[0]
    package = line.split('"')[1]

    print(file, package)

    dotnetcmd = f"dotnet add {file} package {package} --version {args.new_version}"

    cmd = subprocess.Popen(dotnetcmd, shell=True, stdout=subprocess.PIPE, text=True)
    cmd.wait()
    print(cmd.communicate()[0])
