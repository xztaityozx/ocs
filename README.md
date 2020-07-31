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
$ dotnet publish -c Release --self-containd -r linux-x64
# for windows x64
$ dotnet publish -c Release --self-containd -r win-x64
# for macos
$ dotnet publish -c Release --self-containd -r osx-x64

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
```
$ seq 10 | ocs --begin "var s=0" --end "println(s)" "s+=i(F[0])"
55
```

```
seq 10 | ocs "if(i(F0)%2==0) println(F0)"
2
4
6
8
10
```

```
cat ocs/Program.cs | ocs "println(F0.Count(c => c != ' '))"
12
17
22
28
17
34
0
13
21
41
70
67
```

```
seq 1000 | shuf -n 300 | xargs -n3 | ocs --begin "var b=new int[3]" --end "print(b)" "b[0]+=i(F[1]);b[1]+=i(F[2]);b[2]+=i(F[3])"
50092
50452
50144
```


