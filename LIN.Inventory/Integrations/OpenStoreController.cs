namespace LIN.Inventory.Integrations;

[Route("connectors/[controller]")]
public class OpenStoreController : ControllerBase
{

    /// <summary>
    /// Webhook para estados de los pagos en línea relacionados con el inventario.
    /// </summary>
    [HttpPost]
    public IActionResult Webhook()
    {

        // Obtener el external id

        // Buscar la orden

        // Validar orden

        // Cambiar el estado de la reserva a (Finalizado), evitara que se reintegre al inventario.

        // Registar movimiento de la venta en el inventario. (Devolución, venta...)
        /* Cuando un pago se devuelve, el estado del movimiento se actualiza */
        /* Cuando un pago se "paga", el estado del movimiento se actualiza a completado */



        return Ok();
    }

    /// <summary>
    /// Reservar stock de productos en un inventario especifico.
    /// </summary>
    [HttpPost("reserve")]
    public IActionResult Reserve()
    {

        // Reservar stock (true / false)

        /* La reserva tiene una fecha limite, desde de ello, los productos se reintegraran al inventario */



        return Ok();
    }


    /// <summary>
    /// Eliminar una reserva.
    /// </summary>
    [HttpDelete("reserve")]
    public IActionResult DeleteReserve()
    {

        // Eliminar la Reserva del stock

        /* Reintegrar los productos al inventario */

        return Ok();
    }


    /// <summary>
    /// Generar la orden de pago y enlace de pago para una reserva.
    /// </summary>
    [HttpPost("order")]
    public IActionResult Order()
    {

        /* Con el Id de la reserva, se dan 10 minutos mas, para el usuario tener tiempo de pagar, despues, se vence la orden y el enlace de pago */

        // Generar enlace de pago con Payments.

        // Asociar la external Id con la orden local.

        // Crear la orden local.

        // Retornar el enlace de pago.




        return Ok();
    }


    /// <summary>
    /// Buscar productos de uno o varios inventarios conectados con OpenStore.
    /// </summary>
    [HttpGet("search")]
    public IActionResult Search()
    {

        // Buscar productos en todos lo inventarios conectados (O en uno especifico) segun parametro.

        // Buscar por texto, descripcion, categoria.

        // Retornar productos.

        return Ok();
    }

}