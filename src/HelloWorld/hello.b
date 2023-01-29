var result = 0
function test1()
{
	if (result < 0)
	{}
}
function test2()
{
	if (result < 0)
	{}
	return
}

test1()
test2()

