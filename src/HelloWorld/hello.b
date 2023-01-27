var result = 0
for i = 1 to 10
{
    if i/2*2 == i continue
    for j = 11 to 15
    {
        if j == 13 || j == 14 continue
        result = result + i + j
    }
}
println(string(result))
