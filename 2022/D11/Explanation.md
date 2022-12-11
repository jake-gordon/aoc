# Explanation

In contrast to the earlier puzzles of 2022, the difficulty of this puzzle comes from reasoning about types & operations rather than specifics of the implementation.
I will outline my path to the solution in case it's useful for anyone.
The rules surrounding how the monkeys pass around the items are simple, but in Part 2 problems appear because the worry level `w` is no longer reduced by 3 each time.

Depending on how you implemented Part 1 the issue may or may not be obvious.
If you used signed integer types to represent `w` printing the queue of each monkey at the end of a round would reveal that some of them are negative.
Since `w > 0` and the operations `w * c`, `w + c`, `w * w` with `c > 0` don't change the sign of `w` the negative value indicates that the variable has overflown, i.e. reached **ridiculous levels**.
You *could* also suspect this if you used unsigned integer types but it's less obvious since you have to look for values close to the maximum value of `uint`.

Now that we've identified the problem we can upgrade to the shiniest 64-bit `uints` on our machine, but more importantly we need to find a way to reduce the values of `w` **without changing the logic of how they're passed around**.
In the end we're only interested in the number of times each monkey has handled an item, so we can reduce the values of `w` if they're getting too large.
However, we can't modify them arbitrarily since we might change the outcome of the tests `w % d == 0` used to decide who gets the item next or the value of the worry functions which will affect the handling counts.
Therefore, we want to find a transformation `r(w)` reducing the value of `w` that satisfies:

1. `w % d == 0` if and only if `r(w) % d == 0` (reduction does not change throw logic)
2. `r[f(w)] = f[r(w)]` for all inputs (reduction commutes with monkey worry function, `[r,f] = 0`)

One way to find the solution is to think about the modulo operation a bit more.
The operation `% N` where `N` is some integer wraps the first `N` integers back onto themselves, i.e. if we write `w = n*N + m` then we just take the value of `m` in `{0,1,...,N-1}` and discard the part with the integer multiple of `N`.
So the integers are mapped back onto the first `N` numbers and `% N` can act as a reduction
``` 
   w%N                                                w
[ - m ... - ]   [ - - ... - ] ... [ - - ... - ]   [ - m ... - ] ...
0          N-1  N         2N-1  (n-1)N      nN-1  nN       (n+1)N-1
```
If we choose `r(w) = w % N` for some `N`, can we choose the value of `N` to meet all the requirements on `r(w)`?
The second requirement is automatically satisfied for this choice of `r(w)` due to properties of the modulo operator.
In the following we let `a = n1*N + m1` and `b = n2*N + m2`.

1. Adding them we find `a + b = (n1 + n2)*N + (m1 + m2)` which becomes `(a + b) % N  = m1 + m2 = (a % N) + (b % N)`.
This shows that `+` commutes with `% N`.
2. Multiplying them we find `a * b = (n1*n2*N + n1*m2 + m1*n2)*N + m1*m2` which becomes `(a * b) % N = m1 * m2 = (a % N) * (b % N)` after  modulo `N`.
This shows that `*` commutes with `% N`.
3. By repeatedly using the above properties the operations in any polynomial with integer coefficients (each `< N` so that `a%N = a`) also commute with `% N`.
Every monkey's worry function in our problem is a polynomial of degree `<= 2`.

Now let's find an `N` so that for a given `d` the test `w % d == 0` is preserved `(w % N) % d == 0`.
An easy way to do this is to choose `N = d`, since `w % d` is already wrapped into `{0,1,...,d-1}` a second modulo does nothing `(w % d) % d = w % d` and the tests are clearly the same.
This works for a single monkey, but for monkeys with other divisors `d' != d` this won't be true.
A way to generalize is to choose `N` as an integer multiple of `d`, `N = n*d`, and it's not too hard to prove that the first requirement on `r(w)` is satisfied for all inputs:

1. If `w` is smaller than `N` no reduction is performed by `% N` and we're comparing the exact same integer.
2. Otherwise, `w` is larger than `N` we have to wrap the integer back onto `{0,1,...,N-1}`.
When `w` is divisible by `d` we can write `w = m*d` with `m >= n` and the modulo operator maps `m` onto some smaller `m' < n` (in fact, `m' = m % n`).
In this case the reduced `w` is `r(w) = m' * d` and `r(w) % d = (m' * d) % d = 0` is the same as `w % d = 0`.
Otherwise `w = m*d + c` is not divisible by `d` and `r(w) = w % N = m'*d + c` so that `r(w) % d = (m'*d + c) % d  = c` is the same number as `w % d`.

```
 r(w)%d                          r(w) = w%N                                                  w
[ - c ... - ]   [ - - ... - ] ... [ - c ... - ] ...   [ - - ... - ]   [ - - ... - ] ...  [ - c ... - ]
0          d-1  d         2d-1   m'd      (m'+1)d-1  (n-1)d      nd-1 nd       (n+1)d-1  md       (m+1)d-1
[                                                                 ]   [             ...
0                                                                N-1  N
```

Having found a way to satisfy both properties for a single monkey how do we choose `N` so that *all* of the monkey's tests are unaffected by the reduction?
The simplest way is to choose `N` as the product of all the divisors `N = d0 * d1 * ...` so that for each monkey `n` the `N` is an integer multiple of `dn`, i.e. `N = (d0 * d1 * d(n-1) * d(n+1) * ...) * dn`, and all the test logic is unaffected by the reduction.
If we want the *most restrictive* reduction we could choose the least common multiple of the divisors, i.e. the smallest `N` such that it's an integer multiple of each divisor, but this optimization is not necessary for the given inputs.

Switching out the reduction `w => w / 3` for `w => w % N` on each monkey and using 64-bit `uints` in appropriate places is all that's needed to solve Part 2.
