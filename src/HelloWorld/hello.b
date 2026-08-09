function main()
{
	var count = 0
	for i = 2147483647 to 2147483647
	{
		count++
		if (count > 1)
		{
			println("Count greater 1!")
			break
		}
	}

	println("Final count: " + string(count))
}