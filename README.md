# Unity Assets Folder Structure

- Art
    - Physics
    - Materials
    - Models
    - Textures
- Audio 
    - Music
    - Sounds 
- Code
    - Scripts
    - Shaders
- Level
    - Prefabs
    - Scenes
    - UI

# Coding Practices

## Must Have

### Flags
For Flags I would like them to be stored in a struct of that class so something like this:
```c#
public struct ClassNameFlags {
  public const string FLAG1 = "FLAG1"
} 

public class ClassName : NetworkComponent {
  ...
}
```
### Dependencies of Components
For the component dependencies something like the RigidBody network component needing a RigidBody, the dependency of the component should be clearly stated.
```c#
[RequireComponent(typeof(Rigidbody))]
public class NetRigidbody : NetworkComponent {}
```
## Preferences

### Handle Messages
For checking for flags use switch cases so that its a lil bit more readable
```c#
public override void HandleMessage(string flag, string value) {
    switch (flag) {
        case ClassNameFlags.FLAG1:
            if (IsServer) {}
            if (IsClient) {}
            ...
            break;    
    }
}
```
### Don't use public if you can use Serialized private
First and foremost follow coding practices of protected and private variables. If a variable needs to be seen in the editor add the tag SerializedField.
```c#
//Do this
[SerializeField] private GameObject myObject;
[SerializeField] protected bool myBool;
//Dont Do this
public GameObject myObject;
public bool myBool;
```


