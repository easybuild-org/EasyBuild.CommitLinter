module Expect

let inline equal (actual: 'a) (expected: 'a) =
    if actual = expected then
        ()
    else
        failwithf
            $"""Expected

%A{expected}

but got

%A{actual}
"""
