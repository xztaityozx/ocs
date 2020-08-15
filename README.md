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
$ dotnet publish -c Release --self-containd true -p:PublishSingleFile=true -p:PublishReadyToRun=true -r linux-x64
# for windows x64
$ dotnet publish -c Release --self-containd true -p:PublishSingleFile=true -p:PublishReadyToRun=true -r win-x64
# for macos
$ dotnet publish -c Release --self-containd true -p:PublishSingleFile=true -p:PublishReadyToRun=true -r osx-x64

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

### Special variable
#### F
Field array.

**Example**
```sh
$ echo a b c | ocs '{Console.WriteLine($"1: {F[1]}, 2: {F[2]}, 3: {F[3]}, 0: {F[0]}")}'
1: a, 2: b, 3: c, 0: a b c
```

#### F0
Line

**Example**
```sh
$ echo a b c | ocs '{Console.WriteLine($"F0: {F0}, F[0]: {F[0]}")}'
F0: a b c, F[0]: a b c
```

#### NF
Number of fields

**Example**
```sh
$ echo -e "a b c\nd e f g\n1 2 3 4 5" | ocs '{Console.WriteLine($"F0={F0}, NF={NF}")}'
F0=a b c, NF=4
F0=d e f g, NF=5
F0=1 2 3 4 5, NF=6
```

#### NR
Number 
