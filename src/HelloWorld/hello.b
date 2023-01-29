function test(argument:int)
{
    for loopVariable = 1 to 10
    {
        var x = 1
        if loopVariable>argument
        {
            var y = x
        }
        var z = 2 * x
    }
    var ende = true
    if (ende)
        var schluss = false
}
function test2(i:int)
{
    if (i<0)
        i++
}
test(2)
test2(2)

