   open System
   open Akka.Actor
   open Akka.FSharp
   //open MyActors.Abstractions // Include the shared abstractions project

   type ActorCommand =
       | SayHello of string
       | GetStatus

   type ActorResponse =
       | HelloAck of string
       | Status of string
       
   let responseLoggerActor (mailbox: Actor<ActorResponse>) =
    let rec loop () = actor {
        let! response = mailbox.Receive()
        match response with
        | HelloAck message 
        | Status message -> printfn $"Received acknowledgment: %s{message}"
        | _ -> printfn "Unexpected response."
        return! loop ()
    }
    loop ()

   // The actor that handles commands
   let greeterActor (mailbox: Actor<ActorCommand>) =
       let rec loop () = actor {
           let! message = mailbox.Receive()
           match message with
           | cmd ->
               match cmd with
               | SayHello name -> 
                   mailbox.Sender() <! HelloAck $"Hello, %s{name}!"
               | GetStatus -> 
                   mailbox.Sender() <! Status "I'm alive!"
           | _ -> printfn "what?"
           return! loop ()
       }
       loop ()

   [<EntryPoint>]
   let main _ =
       let system = ActorSystem.Create("MyActorSystem")
       let greeter = spawn system "greeter" greeterActor
       let responseLogger = spawn system "responseLogger" responseLoggerActor
       
       // Implement a simple HTTP or gRPC API to handle requests (explained below)
       printfn "Actor system started!"
       greeter.Tell(GetStatus, responseLogger)
       printf "Press button to send message:"
       Console.ReadLine() |> ignore
       greeter.Tell(SayHello "Mike", responseLogger)
       Console.ReadLine() |> ignore
       system.Terminate() |> Async.AwaitTask |> Async.RunSynchronously
       0