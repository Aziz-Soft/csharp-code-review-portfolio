## Context
Implement FizzBuzz from 1 to 20: print "Fizz" for multiples of 3,
"Buzz" for multiples of 5, "FizzBuzz" for both, otherwise the number.

## Version A
See VersionA.cs

## Version B
See VersionB.cs

## Verdict
**Version B is better.**

## Justifications

- **Logic**: Version A requires an explicit check for `i % 15 == 0` to handle
  the combined case. Version B eliminates this by building the result
  incrementally — no special case needed.

- **Extensibility (DRY)**: If we need to add `% 7 == "Bazz"`, Version B
  requires one new `if` block. Version A requires updating multiple branches,
  increasing the risk of missed cases.

- **Readability**: Version B reads like a pipeline — build the string,
  fallback to number if empty. The intent is immediately clear.

- **Performance**: Both are O(n). No meaningful difference at this scale.

- **Style**: `string.Empty` is preferred over `""` per Microsoft .NET
  conventions for clarity and intent.

## Tests
```csharp
// Expected output: 1 2 Fizz 4 Buzz Fizz 7 8 Fizz Buzz 11 Fizz 13 14
//                  FizzBuzz 16 17 Fizz 19 Buzz
```

## Possible Improvement
Extract the rule logic into a dictionary `Dictionary<int, string>` for
full configurability, making it testable and open/closed compliant.
