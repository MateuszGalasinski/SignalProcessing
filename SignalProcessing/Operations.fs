namespace SignalProcessing

module Operations = 
    type OperationType = 
    | Addition = 0
    | Substraction = 1
    | Multiplication = 2
    | Division = 3

    let addPoints points1 points2 = 
        let adder = List.map2 (fun (p1:Point) (p2:Point) -> 
            {
                x = {
                        r = p1.x.r + p2.x.r;
                        i = p1.x.i + p2.x.i;
                    }
                y = {
                        r = p1.y.r + p2.y.r;
                        i = p1.y.i + p2.y.i;
                    }
            })
        adder points1 points2

    let add signal1 signal2 =
        {signal1 with points = (addPoints signal1.points signal2.points)}

    let operate operation signal1 signal2 =
        match operation with 
        | OperationType.Addition -> add signal1 signal2
        //| OperationType.Substraction -> ()
        //| OperationType.Multiplication -> ()
        //| OperationType.Division -> ()



