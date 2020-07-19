# ocs
oneliner csharp

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


