// ELIMINADO — BLL.BitacoraNegocio fue un wrapper innecesario sobre DAL.BitacoraNegocio.
//
// Las consultas de eventos de negocio se hacen directamente desde GUI
// usando Servicios.BitacoraNegocio (ObtenerTodos / BuscarPorFiltros).
//
// Arquitectura correcta:
//   BLL decide CUÁNDO registrar → llama a Servicios.BitacoraNegocio.Registrar()
//   GUI consulta directamente   → Servicios.BitacoraNegocio.ObtenerTodos() / BuscarPorFiltros()
//   DAL persiste y recupera     → solo accede desde Servicios.BitacoraNegocio, nunca desde GUI.
