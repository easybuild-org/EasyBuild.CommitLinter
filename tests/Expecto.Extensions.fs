module Expecto.NoMessage.Expect

open Expecto

let inline equal actual expected = Expect.equal actual expected ""

let inline notEqual actual expected = Expect.notEqual actual expected ""
