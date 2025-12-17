local LrDialogs = import "LrDialogs"
local LrFunctionContext = import "LrFunctionContext"
local LrTasks = import "LrTasks"
local LrApplication = import "LrApplication"
local LrApplicationView = import "LrApplicationView"
local LrSelection = import "LrSelection"
local LrDevelopController = import "LrDevelopController"
local LrSocket = import "LrSocket"
local LrTableUtils = import "LrTableUtils"
local LrPathUtils = import "LrPathUtils"
local LrFileUtils = import "LrFileUtils"
-- ==============================================================================
-- Port numbers
-- port zero indicates that we want the OS to auto-assign the port
local AUTO_PORT = 0
-- port number used to receive commands
local defaultReceivePort = 49000
-- ==============================================================================

-- All of the Develop parameters that we will monitor for changes. For a complete
-- listing of all parameter names, see the API documentation for LrDevelopController.
local develop_params = {"Temperature", "Tint", "Exposure", "Contrast", "Highlights", "Shadows", "Whites", "Blacks", "Texture",
                        "Clarity", "Vibrance", "Saturation", "PresetAmount", "ProfileAmount", -- Tone Panel
"ParametricDarks", "ParametricLights", "ParametricShadows", "ParametricHighlights", "ParametricShadowSplit",
                        "ParametricMidtoneSplit", "ParametricHighlightSplit", -- HSL / Color
"SaturationAdjustmentRed", "SaturationAdjustmentOrange", "SaturationAdjustmentYellow", "SaturationAdjustmentGreen",
                        "SaturationAdjustmentAqua", "SaturationAdjustmentBlue", "SaturationAdjustmentPurple",
                        "SaturationAdjustmentMagenta", "HueAdjustmentRed", "HueAdjustmentOrange", "HueAdjustmentYellow",
                        "HueAdjustmentGreen", "HueAdjustmentAqua", "HueAdjustmentBlue", "HueAdjustmentPurple",
                        "HueAdjustmentMagenta", "LuminanceAdjustmentRed", "LuminanceAdjustmentOrange",
                        "LuminanceAdjustmentYellow", "LuminanceAdjustmentGreen", "LuminanceAdjustmentAqua",
                        "LuminanceAdjustmentBlue", "LuminanceAdjustmentPurple", "LuminanceAdjustmentMagenta",
                        "PointColors", -- B & W
"GrayMixerRed", "GrayMixerOrange", "GrayMixerYellow", "GrayMixerGreen", "GrayMixerAqua", "GrayMixerBlue",
                        "GrayMixerPurple", "GrayMixerMagenta", -- Split Toning
"SplitToningShadowHue", "SplitToningShadowSaturation", "ColorGradeShadowLum", "SplitToningHighlightHue",
                        "SplitToningHighlightSaturation", "ColorGradeHighlightLum", "ColorGradeMidtoneHue",
                        "ColorGradeMidtoneSat", "ColorGradeMidtoneLum", "ColorGradeGlobalHue", "ColorGradeGlobalSat",
                        "ColorGradeGlobalLum", "SplitToningBalance", "ColorGradeBlending", -- detail Panel
"Sharpness", "SharpenRadius", "SharpenDetail", "SharpenEdgeMasking", "LuminanceSmoothing",
                        "LuminanceNoiseReductionDetail", "LuminanceNoiseReductionContrast", "ColorNoiseReduction",
                        "ColorNoiseReductionDetail", "ColorNoiseReductionSmoothness", -- Dehaze
"Dehaze", -- Post-Crop Vignetting
"PostCropVignetteAmount", "PostCropVignetteMidpoint", "PostCropVignetteFeather", "PostCropVignetteRoundness",
                        "PostCropVignetteStyle", "PostCropVignetteHighlightContrast", -- Grain
"GrainAmount", "GrainSize", "GrainFrequency", -- Profile
"LensProfileDistortionScale", "LensProfileChromaticAberrationScale", "LensProfileVignettingScale",
                        "LensManualDistortionAmount", -- Color
"DefringePurpleAmount", "DefringePurpleHueLo", "DefringePurpleHueHi", "DefringeGreenAmount", "DefringeGreenHueLo",
                        "DefringeGreenHueHi", -- Manual Perspective
"PerspectiveVertical", "PerspectiveHorizontal", "PerspectiveRotate", "PerspectiveScale", "PerspectiveAspect",
                        "PerspectiveX", "PerspectiveY", "PerspectiveUpright", -- Calibrate Panel
"ShadowTint", "RedHue", "RedSaturation", "GreenHue", "GreenSaturation", "BlueHue", "BlueSaturation", -- Cropping
"straightenAngle", -- Local Adjustments
"local_Temperature", "local_Tint", "local_Exposure", "local_Contrast", "local_Highlights", "local_Shadows",
                        "local_Clarity", "local_Saturation", "local_ToningHue", "local_ToningSaturation",
                        "local_Sharpness", "local_LuminanceNoise", "local_Moire", "local_Defringe", "local_Blacks",
                        "local_Whites", "local_Dehaze", "local_PointColors", "local_Texture", "local_Hue",
                        "local_Amount", "local_Maincurve", "local_Redcurve", "local_Greencurve", "local_Bluecurve",
                        "local_Grain", "local_RefineSaturation"}

local string_replacements = {"LensProfile", "PostCrop", "Adjustment", "ColorGrade", "SplitToning", "GrayMixer"}
local circle_params = { "SplitToningShadowHue", "SplitToningHighlightHue", "ColorGradeMidtoneHue", "ColorGradeGlobalHue" }
local test_command = "Test"

local develop_param_set = {}
for _, key in ipairs(develop_params) do
    develop_param_set[key] = true
end

local circle_param_set = {}
for _, key in ipairs(circle_params) do
    circle_param_set[key] = true
end

local currentSetting = {
    key = nil,
    value = nil
}

local last_debounce_call = 0
local delay = 2

local function debounce(key, value)
    local now = os.clock()

    if now - last_debounce_call >= delay then
        currentSetting.key = nil
        currentSetting.value = nil
        last_debounce_call = now -- Update the last call time
    else
        last_debounce_call = now
    end

    if currentSetting.key == nil or currentSetting.key ~= key then
        currentSetting.key = key
        currentSetting.value = value
    end
end

local function formatKeyString(input)
    -- Remove underscores
    local modifiedString = string.gsub(input, "_", "")

    for _, element in ipairs(string_replacements) do
        modifiedString = string.gsub(modifiedString, element, "")
    end

    -- Capitalize the first letter
    modifiedString = modifiedString:sub(1, 1):upper() .. modifiedString:sub(2)

    local split = {}

    -- Use gmatch to find all sequences of capital letters
    for word in string.gmatch(modifiedString, "[A-Z][a-z]*") do
        table.insert(split, word)
    end

    -- Merge the result with whitespace
    return table.concat(split, " ")
end

function clamp(num, min, max)
    if num < min then
        return min
    elseif num > max then
        return max
    else
        return num
    end
end

--------------------------------------------------------------------------------
-- Given a key/value pair that has been parsed from a receiver port message, calls
-- the appropriate API to adjust a setting in Lr.
local function setValue(key, value)
    local modName = LrApplicationView.getCurrentModuleName()
    if (modName ~= "develop") then
        return
    end
    if value == "reset" then -- ex: "Exposure = reset"
        LrDevelopController.resetToDefault(key)
        LrDialogs.showBezel(formatKeyString(key) .. " reset", delay)
        return true
    end
    if value:sub(-1) == "%" then
        local numericValue = tonumber(value:sub(1, -2))
        if numericValue then
            if key and develop_param_set[key] then -- ex: "Exposure = 1.5"
                local currentValue = LrDevelopController.getValue(key)

                debounce(key, currentValue)

                local min, max = LrDevelopController.getRange(key)
                local adjusted_max = 100

                if max > 10000 then
                    adjusted_max = max / 10
                elseif max > 1000 then
                    adjusted_max = max / 5
                elseif max > 500 then
                    adjusted_max = max / 2
                elseif max < 100 then
                    adjusted_max = max
                end

                local increment = (numericValue * adjusted_max) / 100
                if increment >= 0 then
                    increment = math.floor(increment * 100 + 0.5) / 100
                else
                    increment = math.ceil(increment * 100 - 0.5) / 100
                end

				local inc_val = currentValue + increment
				
				if circle_param_set[key] then
					inc_val = math.fmod(currentValue + increment + max, max)
				end
				
                local newVal = clamp(inc_val, min, max);

                local prievousValue = currentSetting.value
                local deltaString = "0"
                local newValString = tostring(newVal)

                if prievousValue then
                    local delta = newVal - prievousValue

                    if math.abs(delta) < 0.01 then
                        delta = 0
                    end

                    if math.abs(newVal) < 0.01 then
                        newVal = 0
                        newValString = "0"
                    end

                    local decimal_part = string.format("%.2f", delta):match("%.(%d%d)")
                    local firstDecimal = tonumber(decimal_part:sub(1, 1))
                    local secondDecimal = tonumber(decimal_part:sub(2, 2))

                    deltaString = tostring(delta)

                    if firstDecimal > 0 or secondDecimal > 0 then
                        deltaString = string.format("%.2f", delta)
                        newValString = string.format("%.2f", newVal)
                    end

                    if delta > 0 then
                        deltaString = "+" .. deltaString
                    end
                end

                LrDevelopController.setValue(key, newVal)

                LrDialogs.showBezel(formatKeyString(key) .. " " .. newValString .. " (" .. deltaString .. ")", delay)
                return true
            end
        end
    end
    local numericValue = tonumber(value)
    if numericValue then
        if key and develop_param_set[key] then -- ex: "Exposure = 1.5"
            local currentValue = LrDevelopController.getValue(key)
            local newVal = currentValue + numericValue;
            debounce(key, currentValue)

            local prievousValue = currentSetting.value
            local delta = "0"

            if prievousValue then
                delta = newVal - prievousValue
                if delta > 0 then
                    delta = "+" .. delta
                end
            end
            LrDevelopController.setValue(key, newVal)
            LrDialogs.showBezel(formatKeyString(key) .. " " .. newVal .. " (" .. delta .. ")", delay)
            return true
        end
    end
end

--------------------------------------------------------------------------------
-- Simple parser for messages sent from the external process over the socket
-- connection, formatted as "key = value". (ex: "rating = 2")
local function parseMessage(data)
    if type(data) == "string" then
        local _, _, key, value = string.find(data, "([^ ]+)%s*=%s*(.*)")
        return key, value
    end
end

--------------------------------------------------------------------------------
local receiverPort
local receiverConnected = false
local makingReceiver = false
--------------------------------------------------------------------------------

local function getPortFromFile()
    local thePort = defaultReceivePort
    local pluginFolderPath = LrPathUtils.parent(_PLUGIN.path)
    local portPath = LrPathUtils.child(pluginFolderPath, "port")
    if LrFileUtils.exists(portPath) then
        thePort = LrFileUtils.readFile(portPath)
    end
    return thePort
end

--------------------------------------------------------------------------------
local function makeReceiverSocket(context)
    local thePort = getPortFromFile()
    makingReceiver = true
    LrDialogs.showBezel("Opening speedLR at port " .. thePort, 1)
    local receiver = LrSocket.bind {
        functionContext = context,
        port = thePort,
        mode = "receive",
        plugin = _PLUGIN,
        onConnecting = function(socket, port)
            receiverPort = port
        end,
        onConnected = function(socket, port)
            receiverConnected = true
            makingReceiver = false
        end,
        onClosed = function(socket)
            -- If the other side of this socket is closed,
            -- tell the run loop below that it should exit.
            -- _G.running = false
            receiverConnected = false
            makingReceiver = false
        end,
        onMessage = function(socket, message)
            if type(message) == "string" then
                if message == test_command then
                    return
                end
                local key, value = parseMessage(message)
                if key and value then
                    if setValue(key, value) then
                        -- LrDialogs.showBezel( string.format( "%s %s!!!", tostring( key ), tostring( value ) ), 4 )
                    else
                        LrDialogs.showBezel("Unknown command: \"" .. message .. "\"", 1)
                    end
                else
                    LrDialogs.showBezel("Unknown command: \"" .. message .. "\"", 1)
                end
            end
        end,
        onError = function(socket, err)
            if err == "timeout" then
                socket:reconnect()
            end
        end
    }
    -- automatically scroll sliders into view whenever they are adjusted
    LrDevelopController.revealAdjustedControls(true)
    return receiver
end

--------------------------------------------------------------------------------
-- Start everything in an async task so we can sleep in a loop until we are shut down.
LrTasks.startAsyncTask(function()
    local pluginFolderPath = LrPathUtils.parent(_PLUGIN.path)
    local exePath = LrPathUtils.child(pluginFolderPath, "SpeedLR.exe")
    if LrFileUtils.exists(exePath) then
        LrTasks.execute("cd "..pluginFolderPath .. "& SpeedLR.exe")
    end
end)

LrTasks.startAsyncTask(function()
    -- A function context is required for the socket API below. When this context
    -- is exited all socket connections that have been created from it will be
    -- closed. We stay inside this context indefinitiely by spinning in a sleep
    -- loop until told to exit.
    LrFunctionContext.callWithContext('socket_remote', function(context)
        -- Loop until this plug-in global is set to false, which happens when the external process
        -- closes the socket connection(s), or if the user selects the menu command "File >
        -- Plug-in Extras > Stop" , or when Lightroom is shutting down.
        _G.running = true
        while _G.running do
            if receiverConnected == false and makingReceiver == false then
                makeReceiverSocket(context)
            end
            LrTasks.sleep(1 / 2) -- seconds
        end
        _G.shutdown = true
        if receiverConnected then
            receiver:close()
        end
        LrDialogs.showBezel("Remote connection closed", 4)
    end)
end)


