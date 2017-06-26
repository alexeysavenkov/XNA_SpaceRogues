namespace FSharpPart

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

module Hint =

    let private hints = [| "If you shoot flying rocket\n you'll have a boom.";
                            "If two flying rockets collide\n there will be a very big boom.";
                            "When enemy ship gets to the right side\n you lose less score if it is damaged.";
                            "Look at the colors! You lose score when you attack ally ships.";
                            "Note that when ships fly uncontrollably\n they can ram other ships.";
                            "Percents are multiplied, not added.\nFor example, if you get 60% and 30% bonuses\n for the same skill\n you will have 108% bonus, not 90%";
                            "When ship is destroyed, it explodes and can damage nearby ships.";
                            "Direct rocket hit destroys ship"|]

    let private random = new System.Random()

    let private next() = 
                let index = random.Next(Array.length hints)
                hints.[index] 

    type HintObj(Position:Vector2, Font:SpriteFont) =
        let lines = 
            next().Split('\n') |> Array.append [|"Random hint:"|]
            |> Array.mapi(fun i x -> x, let meas = Font.MeasureString(x) in 
                                        new Vector2(Position.X - meas.X/2.0f, Position.Y + 25.f*(float32 i)))           
            
        member this.Draw(batch:SpriteBatch) =
            lines |> Array.iter(fun (str, pos) -> batch.DrawString(Font, str, pos, Color.White*0.6f))
        
    let mutable curObj : HintObj option  = None

    let Create Position Font =
        curObj <- Some(new HintObj(Position, Font))

    let Draw(batch:SpriteBatch) =
        curObj.Value.Draw(batch)