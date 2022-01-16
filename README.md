# A set of roslyn analyzers for convinient use of ***Unity coroutines***

#### This package was designed to help to prevent you from common issues when using unity coroutines.

### [Nuget Package](https://www.nuget.org/packages/UnityCoroutinesAnalyzer/1.0.3.5)

## Analyzers

### **1. Coroutine invocation analyzer**

As You know there is few ways to start a coroutine in unity:
  - Passing coroutine name:
  
    ```csharp
      StartCoroutine("Coroutine");
       
      IEnumerator Coroutine() {}
    ```
  - Executing coroutine method:
  
    ```csharp
      StartCoroutine(Coroutine());
       
      IEnumerator Coroutine() {}
    ```
  </br>
  
  > #### Problems
  Using the first one it is easy to make a mistake in coroutine name when writing string literal.

  And if you would use the second one, you won't be able to stop the coroutine. Because you are creating an independent coroutine instance.
  
  </br>
  
  > #### Solution
  - The first analyzer encourages you to start the coroutine using 'nameof' syntax:
  
    ```csharp
      StartCoroutine(nameof(Coroutine));
       
      IEnumerator Coroutine() {}
    ```
    So now you are able to call 'StopCoroutine' method with the expected result and avoid excessive mistakes.
    
 
### **2. Yield return in `while(true)` block**

  Coroutines in Unity are often used in combinations with `while(true)` block: 
  ```csharp
    IEnumerator Coroutine()
    {
      while(true)
      {
        ...
      }
    }
  ```
  </br>
  
  > #### Problems
  
  Although it is easy to forget to put `yield return` statement inside `while(true)` (that leads to Unity crash), 
  Visual Studio doesn't have any embedded analyzers that prevents you from such troubles. So the code above won't trigger any Visual Studio warnings or     recommendations. 
  </br>
  
  > #### Solution
  Code analyzers in this package would warn you about possible issues and suggest to put `yield return null;` on the end of `while` block:
  
  ```csharp
    IEnumerator Coroutine()
    {
      while(true)
      {
        ...
        yield return null;
      }
    }
  ```
