namespace SignalProcessing

module Operations = 
    type OperationType = 
    | Addition = 0
    | Substraction = 1
    | Multiplication = 2
    | Division = 3

    let processPoints operation points1 points2 = 
        let processer = List.map2 (fun (p1:Point) (p2:Point) -> 
            {
                x = {
                        r = operation p1.x.r p2.x.r;
                        i = operation p1.x.i p2.x.i;
                    }
                y = {
                        r = operation p1.y.r p2.y.r;
                        i = operation p1.y.i p2.y.i;
                    }
            })
        processer points1 points2

    let add signal1 signal2 =
        {signal1 with points = (processPoints (fun a b -> a + b) signal1.points signal2.points)}

    let substract signal1 signal2 =
        {signal1 with points = (processPoints (fun a b -> a - b) signal1.points signal2.points)}
    
    let divide signal1 signal2 =
        {signal1 with points = (processPoints (fun a b -> a / b) signal1.points signal2.points)}

    let multiply signal1 signal2 =
        {signal1 with points = (processPoints (fun a b -> a * b) signal1.points signal2.points)}

    let operate operation signal1 signal2 =
        match operation with 
        | OperationType.Addition -> add signal1 signal2
        | OperationType.Substraction -> substract signal1 signal2
        | OperationType.Multiplication -> divide signal1 signal2
        | OperationType.Division -> multiply signal1 signal2



