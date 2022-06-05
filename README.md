<details open="open">
  <summary>Table of Contents</summary>
  <ol>
    <li><a href="#about-the-project">About the project</a></li>
    <li><a href="#To-run-the-example-application">To run the example application</a></li>
    <li><a href="#Built-with">Built with</a></li>
    <li><a href="#contributing">Contributing</a></li>
    <ul>
        <li><a href="#Future-work">Future work</a></li>
    </ul>
    <li><a href="#background">Background</a></li>
    <li><a href="#Authors">Authors</a></li>
    <li><a href="#license">License</a></li>
  </ol>
</details>


# About the project

Demo code of a continuation monad and equivalent ALA functionality. Code used for online book at abstractionlayeredarchitecture.com 

The purpose of this project is example code to compare monads and ALA ([Abstraction Layered Architecture](AbstractionLayeredArchitecture.md)).

It's not intended as a useful implementation of a Continuation monad.
It's to show how monads work to compose function with actual code, so that this can be compared with how ALA composes things and hows its code works.
This is all to show that ALA is both more flexible/powerful and simpler than using monads.

This example is one of a set of examples implementing different types of monads.

If you don't understand monads, the section in chapter 3 explains monads. All other attempts to explain monads that I have found have failed in my opinion.
It wasn't until I implemented them myself with this set of examples (for the purpose of comparing what they do and how they work with ALA) that
I realised that all the previous explanations I had read had been inadequate. So go onto the ALA  web site <http://www.abstractionlayeredarchitecture.com> and have a read if you really want to understand monads.

  
## To run the demo monad application

1. Clone this repository or download as a zip.
2. Open the solution in Visual Studio 2019 or later
3. Import the needed nuget package (which is required only to get async/await working properly using a single thread in a console application).
4. When the application runs, you will see the program output the input to the first monad.
5. The first monad does a delay and then you see its output. Then you see the input to the second monad which does I/O.
6. Enter a number and the second monad will then finish.


## Built with

C#, Visual Studio 2019


## Contributing

Contributions are what make the open source community such an amazing place to be learn, inspire, and create. Any contributions you make are **greatly appreciated**.

1. Fork the project using the button at top right of the main Github page or (<https://github.com/johnspray74/ALAExample/fork>)
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -am 'Add AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request


### Future work

If someone who expert with C# would like to look over my code and improve it that would be appreciated.
If someone who is an expert with Haskel, closure, Swift, Java, Python or Rust would like to contribute a version of this example code that would be great.

## Background


## Authors

John Spray

### Contact

John Spray - johnspray274@gmail.com



## License

This project is licensed under the terms of the MIT license. See [License.txt](License.txt)

[![GitHub license](https://img.shields.io/github/license/johnspray74/ALAExample)](https://github.com/johnspray74/ALAExample/blob/master/License.txt)

## Acknowledgments


