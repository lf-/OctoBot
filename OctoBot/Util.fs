module Util
let (|>>) xA x2y = async.Bind (xA, x2y >> async.Return)

