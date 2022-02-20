# Tests suite for ARC-0017

ARC: https://github.com/algorandfoundation/ARCs/pull/67/files

![Tests](tests.png?raw=true "Tests VS")

## How to run

1. Install and run algorand sandbox

```git clone https://github.com/algorand/sandbox.git```

```cd sandbox && ./sandbox up```

2. Get sandbox account mnemonics

```./sandbox goal address export -a {your sandbox address}```

3. Update appsettings.json file

Set up your environmental variables such as SandboxAcc1Mnemonic or deposit to GCOBIVIJIDA7ZWYYP7IBJBRR442PMCCFJF6UZ3CICO6IARZ2JMRKPEGTQQ sandbox algos.

4. Run tests in Visual studio
