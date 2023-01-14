function output(age : int, name : string)
{
	print(name + " is ")
	print(age)
	println(" years old.")
}

function getName() : string
{
	print("Your name: ")
	return input()
}

function getAge(name : string) : int
{
	println("Hi, " + name)
	print("How old are you: ")
	return int(input())
}

function main()
{
	var name = getName()
	var age = getAge(name)
	output(age, name)

	println("A random number between 0 and " + string(age) + ": " + string(random(age)))
}