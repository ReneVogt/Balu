function showRandom(i:int)
{
   print(string(i) + ". Zahl: ")
   println(random(10))
}
function main()
{
	for i=1 to 10
		showRandom(i)
	println("Done.")
}