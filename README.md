# ocs
ocs is **o**neliner **cs**harp


## Install

### Build from source
```
# clone this repository
$ git clone https://github.com/xztaityozx/ocs
$ cd ocs/ocs

# build ocs
# for linux x64
$ dotnet publish -c Release --self-containd true -r linux-x64
# for windows x64
$ dotnet publish -c Release --self-containd true -r win-x64
# for macos
$ dotnet publish -c Release --self-containd true -r osx-x64

# make alias
$ alias ocs=$PWD/bin/Release/netcoreapp3.1/publish/linux-x64/ocs
$ alias ocs=$PWD/bin/Release/netcoreapp3.1/publish/win-x64/ocs.exe
$ alias ocs=$PWD/bin/Release/netcoreapp3.1/publish/osx-x64/ocs

# or add path to `PATH` environment
$ export PATH="${PATH}:$PWD/bin/Release/netcoreapp3.1/publish/linux-x64/ocs"
```

### Download binary from GitHub Releases
Download standalone binary from [release page](https://github.com/xztaityozx/ocs/releases)

## Usage
### Basic usage
```
$ seq 10 | ocs 'BEGIN{var sum=0}{sum+=int.Parse(F0)}END{Console.WriteLine(sum)}'
55
```

The code after **BEGIN** is begin block code. The code after **END** is end block code. These run once before/after main loop(like `awk`'s BEGIN, END). 


