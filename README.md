# ocs
![.NET Core CI/CD](https://github.com/xztaityozx/ocs/workflows/.NET%20Core%20CI/CD/badge.svg?branch=master)

ocs is **o**neliner **cs**harp

## Install

### Build from source
```sh
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

### Built-in variable
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
Number of line

**Example**
```sh
$ yes | head | ocs '{Console.WriteLine(NR)}'
1
2
3
4
5
6
7
8
9
10
```

#### Env
Environment dictionary.

```sh
$ echo | ocs '{Console.WriteLine(Env["LANG"])}'
ja_JP.UTF-8
```

### Built-in functions
#### i(), d()
`i()` is alias to `int.Parse`

**Example**
```sh
$ echo 100 | ocs '{Console.WriteLine($"int.Parse: {int.Parse(F0)+10}, i: {i(F0)+20}")}'
int.Parse: 110, i: 120
```

`d()` is alias to `decimal.Parse([str] ,NumberStyles.Float, CultureInfo.CurrentCulture.NumberFormat)`

**Example**
```sh
$ echo 10 1.1 1e9| ocs '{Console.WriteLine($"{d(F[1])}, {d(F[2])}, {d(F[3])}")}'
10, 1.1, 1000000000
```

#### print(), println()
Shorthand to `Console.Write/Console.WriteLine`

**Example**
```sh
seq 5 | ocs '{Console.WriteLine($"Console.WriteLine: {F0}"); println($"println: {F0}")}'
Console.WriteLine: 1
println: 1
Console.WriteLine: 2
println: 2
Console.WriteLine: 3
println: 3
Console.WriteLine: 4
println: 4
Console.WriteLine: 5
println: 5
```

### Pattern and Action

**Example**

```sh
$ seq 10 | ocs 'i(F0)%2==0{println(F0)}'
2
4
6
8
10
```

```sh
$ seq 10 | ocs 'F0.Length==1{println(F0)}'
1
2
3
4
5
6
7
8
9
```

## Options
###  `-I, --imports`    using Assembles

**Example**
```sh
ocs -ISystem.IO '{var sr = new StreamReader("file")}'
```

###  `-F, --field`      (Default: ) Field separator

**Example**
```sh
cat csv | ocs -F, '{println(F[1])}'
```

###  `-f, --file`       target file

**Example**
```sh
ocs -F, -f csv '{println(F[1])}'
```

###  `--env`            load global environments

**Example**
```sh
$ echo | ocs '{Console.WriteLine(Env["LANG"])}'
ja_JP.UTF-8
```

###  `--show`           (Default: false) show generated code

**Example**
```sh
seq 100 | ocs --show 'BEGIN{var sum = 0;}END{println(sum)}{sum+=i(F0)}'
[ 12:15:47 | Information ] Generated Code
var sum = 0;
using(Reader) while(Reader.Peek() > 0) {
F0 = Reader.ReadLine();
sum+=i(F0);
}
println(sum);

5050
```
