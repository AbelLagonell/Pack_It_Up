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

## Flags
For Flags I would like them to be stored in a struct of that class so something like this:
``` c#
public struct ClassNameFlags {
  public const string FLAG1 = "FLAG1"
} 

public class ClassName : NetworkComponent {
  ...
}
```
## Dependencies of Components
For the component dependencies something like the RigidBody network component needing a RigidBody, the dependency of the component should be clearly stated.
```c#
[RequireComponent(typeof(Rigidbody))]
public class NetRigidbody : NetworkComponent {}
```

