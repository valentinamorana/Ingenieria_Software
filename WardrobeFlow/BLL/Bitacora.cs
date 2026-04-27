// ELIMINADO — BLL.Bitacora fue un wrapper innecesario sobre DAL.Bitacora.
//
// Las consultas de bitácora del sistema se hacen directamente desde GUI
// usando Servicios.Bitacora (ObtenerTodos / ObtenerUltimosNDias / BuscarPorFiltros).
//
// Arquitectura correcta:
//   BLL decide CUÁNDO registrar → llama a Servicios.Bitacora.Registrar()
//   GUI consulta directamente   → Servicios.Bitacora.ObtenerTodos() / BuscarPorFiltros()
//   DAL persiste y recupera     → solo accede desde Servicios.Bitacora, nunca desde GUI.
