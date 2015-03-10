module StockStepDefinitions

type StockItem = { Count : int }

open TickSpec
open NUnit.Framework

let mutable stockItem = { Count = 0 }

let [<Given>] ``a server (.*)``  (serverName:string) =
    ()

let [<Given>] ``a website (.*) on server (.*)`` (siteName:string, serverName:string) =
    ()
    //stockItem <- { stockItem with Count = n }

let [<When>] ``I request a new website (.*) on server (.*)`` (siteName:string, serverName:string) =
    ()
    //stockItem <- { stockItem with Count = stockItem.Count + 1 }

let [<Then>] ``a new website (.*) should be added to server (.*)`` (siteName:string, serverName:string) =
    ()
    //let passed = (stockItem.Count = n)
    //Assert.True(passed)

let [<Then>] ``a new binding (.*) should be added to website (.*) on server (.*)`` (bindingName:string, siteName:string, serverName:string) =
    ()
    //let passed = (stockItem.Count = n)
    //Assert.True(passed)

let [<Then>] ``no website should be created`` () =
    ()
    //let passed = (stockItem.Count = n)
    //Assert.True(passed)