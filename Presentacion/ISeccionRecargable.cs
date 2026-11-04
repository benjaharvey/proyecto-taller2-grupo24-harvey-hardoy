namespace Presentacion;

// Implementado por los ViewModels que deben releer sus datos cuando el usuario
// vuelve a su sección, para no depender de un botón de "actualizar".
public interface ISeccionRecargable
{
    void Recargar();
}
