#r @"C:\Users\anthyme\.nuget\packages\fsharp.data\3.0.0\lib\netstandard2.0\FSharp.Data.dll"
#r @"C:\Users\anthyme\.nuget\packages\xplot.plotly\1.5.0\lib\net45\XPlot.Plotly.dll"

open System
open System.IO
open System.Text.RegularExpressions
open XPlot.Plotly

Environment.CurrentDirectory <- __SOURCE_DIRECTORY__


let (|Scene|Character|Word|) (text:string) =
    if text.Contains "INT." ||  text.Contains "EXT." then Scene text
    elif Regex.Match(text, "^[A-Z0-9 ]+$").Success then Character text
    else Word

let removeB (text:string) = text.Replace("<b> ", "").Replace("<b>", "").Trim()

let (<&>) f g = (fun x -> f x && g x)
let (<|>) f g = (fun x -> f x || g x)

let tooManyWords (text:string) = text.Split[|' '|] |> Seq.length |> ((<=) 4)
let isSpecific (text:string) = 
    ["BLACK SCREEN";"FADE TO BLACK";"MERRY AND PIPPIN";"ELROND SURVEYS THE GROUP"; "FRODO SMILES";"FRODO DISAPPEARS"]
    |> Seq.contains text


let rec parse scenes characters (items:string list) =
    match items with
    | head::rest ->
        match head with
        | Scene title -> 
            let previousScene = 
                List.rev characters 
                |> List.filter (not << (tooManyWords <|> isSpecific))
            parse (previousScene::scenes) [] rest
        | Character name -> 
            parse scenes (removeB name::characters) rest
        | Word ->
            parse scenes characters rest
    | [] -> List.rev scenes
    

let countDialogs scenes =
    scenes
    |> Seq.collect id
    |> Seq.groupBy id
    |> Seq.map (fun (name, elements) -> (name, Seq.length elements))
    |> Seq.toList


File.ReadAllLines "lor1.html" 
|> Seq.map removeB
|> Seq.toList
|> parse [] []
|> countDialogs
|> Seq.sortByDescending snd
|> Seq.take 10
|> Chart.Bar
|> Chart.WithTitle("Qui parle le plus dans le LoR 1")
|> Chart.Show

File.ReadAllLines "lor3.html" 
|> Seq.map removeB
|> Seq.toList
|> parse [] []
|> countDialogs
|> Seq.sortByDescending snd
|> Seq.take 10
|> Chart.Bar
|> Chart.WithTitle("Qui parle le plus dans le LoR 3")
|> Chart.Show

let script = File.ReadAllLines "lor1.html" |> Seq.map removeB
let scenes = parse [] [] (List.ofSeq script) 
let counts = scenes |> countDialogs