function test(argument:int)
{
    for loopVariable = 1 to 2
    {
        var x = 1
        if loopVariable>argument
        {
            var y = x
        }
        var z = 2 * x
    }

    var i = 0
    while(i < 3)
    {        
        var x = 2
        i++
        var y = 5
    }
    do
    {
        var z = 5
        i--
    } while(i > 0 )
    var ende = true
    if (ende)
        var schluss = false
}
function test2(i:int)
{
    if (i<0)
        i++
}
test(1)
test2(2)

