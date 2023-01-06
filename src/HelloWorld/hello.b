function main()
{
	showName(getName())
}

function getName() : string
{
  print("Your name: ")
  return input()
}

function showName(name : string)
{
	print("Hello " + name + "!\r\n")
}
