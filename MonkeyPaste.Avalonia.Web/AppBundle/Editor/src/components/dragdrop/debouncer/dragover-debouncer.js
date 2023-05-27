// #region Life Cycle

// #endregion Life Cycle

// #region Getters

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

// #endregion State

// #region Actions

function resetDebounceDragOver() {
	globals.LastDebouncedMouseLoc = null;
	globals.LastDragOverDateTime = null;
}

function canDebounceDragOverProceed() {
	// DEBOUNCE (my own type but word comes from https://css-tricks.com/debouncing-throttling-explained-examples/)

	let cur_date_time = Date.now();
	if (globals.LastDragOverDateTime == null) {
		globals.LastDragOverDateTime = cur_date_time;
	}

	if (globals.LastDebouncedMouseLoc == null) {
		globals.LastDebouncedMouseLoc = globals.WindowMouseLoc;
		// this is the first call, let it go
		return true;
	}

	// get dist since last proceed
	let m_delta_dist = dist(globals.LastDebouncedMouseLoc, globals.WindowMouseLoc);

	// get time (ms) since last proceed
	let m_dt = globals.LastDragOverDateTime - cur_date_time;
	// get velocity since last call
	let m_v = m_delta_dist / m_dt;

	let can_proceed = m_delta_dist != 0 || m_v != 0;
	if (can_proceed) {
		// update dist checker
		globals.LastDebouncedMouseLoc = globals.WindowMouseLoc;
	}
	// update velocity checker
	globals.LastDragOverDateTime = cur_date_time;

	return can_proceed;
}
// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers