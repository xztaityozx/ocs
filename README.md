# ocs
![.NET Core CI/CD](https://github.com/xztaityozx/ocs/workflows/.NET%20Core%20CI/CD/badge.svg?branch=main)

ocs is **o**neliner **cs**harp

## Install

### Build from source
```sh
# clone this repository
$ git clone https://github.com/xztaityozx/ocs
$ cd ocs/ocs

# build ocs
$ make publish

# install ocs to $HOME/.local/bin
$ make install
# example) install to /usr/loca/bin/
$ PREFIX=/usr/local/bin/ make install
```

### Download binary from GitHub Releases
Download standalone binary from [release page](https://github.com/xztaityozx/ocs/releases)

## Uninstall

```sh
$ make uninstall
```

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
###  `-U, --using-list`    using Assembles

**Example**
```sh
$ ocs -U System.IO '...'

# multiple
$ ocs -U System.IO,System.Collections.Generic '...' 
```

###  `-d, --input-delimiter`      (Default: ) 
**Example**
```sh
cat csv | ocs -d, '{println(F[1])}'
```

###  `-f, --file`       input file

**Example**
```sh
ocs -d, -f csv '{println(F[1])}'
```

### `-D, --output-delimiter`      (Default: )
**Example**
```sh
cat csv | ocs -d, -D : '{println(F)}' 
```

### `-R, --reference-list`      list of reference path
**Example**

```sh
ocs -R YourDll.dll '...'
```

### `--language-version`        (Default: C# 10) set language version
set compile language version

### ` -r, --remove-empty`        (Default: false) remove empty entries from inputs

```sh
$ echo "a   b   c    d" | ocs "{println(F[1], F[4])}"
a b

$ echo "a   b   c    d" | ocs -r "{println(F[1], F[4])}"
a d
```

### ` -g, --use-regexp`          (Default: false) use regular expressions for input delimiter

```sh
$ echo "a   b   c   d" | ocs -gd'\s+' "{println(F[1], F[4])}"
a d
```

### ` --print-generated`         (Default: false) print generated code and exit

```sh
$ ocs --print-generated "{println(F0)}"
public class Runner : IRunner
{
    private Global global;
    public Runner(Global global) => this.global = global;
    private string Ofs => global.Ofs;
    private int NR => global.NR;
    private int NF => global.NF;
    private List<string> F => global.F;
    private string F0 => global.F0;
    private void print(params object[] o) => global.Print(Global.PrintOption.None, o);
    private void println(params object[] o) => global.Print(Global.PrintOption.Line, o);
    private int i(string s) => global.i(s);
    private decimal d(string s) => global.d(s);
    // Entry point
    public void Run()
    {
        while (global.NextLine())
        {
            println(F0);
        }
    }
}
```


