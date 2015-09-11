(*** hide ***)
#r "packages/FSharp.Formatting.2.10.0/lib/net40/RazorEngine.dll"
#r "packages/FSharp.Formatting.2.10.0/lib/net40/FSharp.MetadataFormat.dll"
open FSharp.MetadataFormat
open System
open System.Net
open System.IO
open System.Text.RegularExpressions
open RazorEngine

(** 
The Anagram Problem
===================
This is the problem about the optimal way to find the anagrams of a single English word.  I was asked it at an interview.  I came up with the approach to bucket the dictionary according to 
word length and confine a raw iterative search to those anagrams with the same length as the word you are interested in.  Described below is the approach where each word has the same key.

This is also an exercise in the use of [FSharp.Formatting](http://tpetricek.github.io/FSharp.Formatting/), [GitHub Project Pages](https://pages.github.com/) and [F#](http://fsharp.org/) itself, all of which I am learning.  

*)

(** 
Download a dictionary
---------------------

I found [this one](http://www-personal.umich.edu/~jlawler/wordlist) with a very modest amount of searching.  I'm sure there's others out there.

*)
/// download a dictionary and split it up in to an array of words
let url = new Uri("http://www-personal.umich.edu/~jlawler/wordlist")
let req = WebRequest.Create(url) :?> HttpWebRequest
let reader = new StreamReader(req.GetResponse().GetResponseStream())
let dictWords = reader.ReadToEnd().Split([|"\r\n"|], StringSplitOptions.None);

(** 
Create the key array
--------------------

The way this works is all anagrams will have the same key.  The key is the letters in the character array of the word in question, alphabetically ordered.  For example: consider the 
two words 'deal' and 'lead', they share the key 'adel'.  Given 'lead' you can assemble its key 'adel' then search the key array for 'adel', any matches are anagrams. 

*)
/// create the key array
let keyArray = 
    dictWords
    |> Array.map(fun w -> (Array.sort(w.ToCharArray()) |> System.String), w)

(** 
Filter the array
----------------
Not all alphabetic rearrangements of the dictionary words will be actual words.  Put in some rules to 
get rid of some of them.  No 'q's' unless followed by 'u's' and so on.

*)

/// get rid of some of the impossible anagrams, this could be enhanced, or not depending on perf,
/// conceptually it makes sense to trim the list but I haven't done the measurements 
/// so this is only a partial implementation
let (|ImpossibleAnagram|_|) (pattern: string) (input: string) = 
    let result = Regex.Match(input, pattern)
    
    if result.Success then
        Some input
    else
        None
        
let parseAnagram input = 
    match input with
    | ImpossibleAnagram "q[^u]" input | ImpossibleAnagram @"^kk" input
        -> false
    | _ -> true

let probableKeys = 
    keyArray
    |> Array.filter(fun f -> parseAnagram (fst f))
    |> Array.sort

(** 
Find an anagram
---------------

Pass in any word and get its anagrams.  For the time being I'm presuming the word is in the 
word list I'm using.  

*)

let FindAnagrams (word : string) = 
    let key = Array.sort(word.ToCharArray()) |> System.String
    let anagrams = 
        probableKeys
        |> Array.filter(fun x -> fst x = key)
    anagrams

(** 
Next steps
----------

- Put some unit tests in
- Find out the recommended way to test performance in F#
- Put up some perf numbers.
- Implement the word length bucket optimization
- Try out some ordering or segmenting on the probableKey array as it will mostly be in huge buckets of vowels if ordered by key (as currently). 

*)