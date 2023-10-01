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
	if (class_attrb &&
		globals.AllClassAttributes.filter(x => x.attrName == deltaAttrName).length == 0) {
		globals.AllClassAttributes.push(class_attrb);
	} 
	return class_attrb;
}

function registerPlainAttributor(deltaAttrName, keyName, scopeType, suppressWarning = false) {
	let config = { scope: scopeType };
	let plain_attrb = new globals.Parchment.Attributor(deltaAttrName, keyName, config);

	Quill.register(plain_attrb, suppressWarning);
	if (plain_attrb &&
		globals.AllPlainAttributes.filter(x => x.attrName == deltaAttrName).length == 0) {
		globals.AllPlainAttributes.push(plain_attrb);
	} 
	return plain_attrb;
}


// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers