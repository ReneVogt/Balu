function main()
{
	var name = getName()
	print("Hello " + name + "!\r\n")
}

function getName() : string
{
  print("Your name: ")
  return input()
}
