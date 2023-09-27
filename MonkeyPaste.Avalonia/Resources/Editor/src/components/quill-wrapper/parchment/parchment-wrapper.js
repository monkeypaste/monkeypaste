// #region Life Cycle

// #endregion Life Cycle

// #region Getters

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

// #endregion State

// #region Actions

function registerClassAttributor(deltaAttrName, htmlClassName, scopeType, suppressWarning = false) {
	let config = { scope: scopeType };
	let class_attrb = new globals.Parchment.ClassAttributor(deltaAttrName, htmlClassName, config);

	Quill.register(class_attrb, suppressWarning);
	return class_attrb;
}


// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers