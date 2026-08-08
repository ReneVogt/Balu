function main()
{
	var a = bool(test()) && false
	println(a)
}

function test() : any
{
	println("test called.")
	return true
}