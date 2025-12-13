# XUnity.LLMEndpoint

This is a fork from [joshfreitas1984/XUnity.AutoTranslate.LlmTranslators](https://github.com/joshfreitas1984/XUnity.AutoTranslate.LlmTranslators)

I fork this because I want to combine all config into 1 file (configLLM.yaml) personally for convenience. So it won't support with the same config with joshfreitas1984 branch. Latest version is on [release](https://github.com/gickass/XUnity.LLMEndpoint/releases/tag/Release)

If you are here only for missing config file fix, you can download [1.01](https://github.com/gickass/XUnity.LLMEndpoint/releases/tag/Fix) version [here](https://github.com/gickass/XUnity.LLMEndpoint/releases/tag/Fix) 

Tested Online endpoint:
- [OpenAI](https://platform.openai.com/)
	- Probably the most popular LLM that has the highest quality but is not Free. Write `api.openai.com` in url config to use it
- [DeepSeek](https://www.deepseek.com/)
	- Great and cheap for translation. Write `api.deepseek.com` in url config to use it
- [Chutes AI](https://chutes.ai/app/api)
	- Not recommended because unity send a lot request for text translation. After few seconds, you will get "Too Many Requests" (HTTP 429) from this endpoint  . Write `llm.chutes.ai` in url config to use it
 
Local endpoint
- [Ollama Models](https://ollama.com/)
	- Ollama is a local hosting option for LLMs. You are able to run one or more llms on your local machine of varying size. This option is free but will require you to engineer your prompts dependant on the model and/or language. Write `localhost:11434` in url config to use it
- [KoboldCpp](https://github.com/LostRuins/koboldcpp)
	- Good for local hosting. Write `127.0.0.1:500` in url config to use it
 - [Text Generation Web UI](https://github.com/oobabooga/text-generation-webui)
 	- Good for local hosting. Write `localhost:5001` in url config to use it

Not tested but may work (A lot of endpoint support OpenAI-compatible URL)
- [NanoGpt](https://nano-gpt.com/api)
	- It has rate limit 25 per second of each request. Use `nano-gpt.com` in url config to use it
- [OpenRouter](https://openrouter.ai)
 	- Have a lot of model Use `openrouter.ai` in url config to use it
- [Z-ai](https://z.ai/)
 	- They create GLM model. Use `api.z.ai` in url config to use it
- [Nvidia LLM](https://docs.api.nvidia.com/)
	-  Nvidia LLM services. Some are free but still require account. Write `integrate.api.nvidia.com` in url config to use it

# Why use this instead of the [Custom] endpoint?

- You can set any number translations in parallel (unlike the custom endpoint which is tied to 1)
	- But I will just recommend to set it or leave it to 5 if you use local endpoint. 
- We have removed the spam restriction (which has 1 second by default on custom)

# Installation instructions

1. Download or Build the latest assembly from the Releases
2. Install [XUnity.AutoTranslator](https://github.com/bbepis/XUnity.AutoTranslator) into your game as normal with either ReiPatcher or BepinEx
3. Place assembly into Translators folder for your game, you should see the other translators in the folder (eg. CustomTranslate.dll)
	- If used ReiPatcher: `<GameDir>/<Game name>_ManagedData/Translators` 
	- If used BepinEx: `<GameDir>/BepinEx/plugins/XUnity.AutoTranslator/Translators` 

# Configuration

We use an additional yaml configuration file to make it easier to copy around with multiple games.

To configure your LLM you will need to follow the following steps:

1. Either run the game to create the default config or copy "ConfigLLM.yaml" from [Sample Configs](./XUnity.AutoTranslator.LlmTranslators/SampleConfig) into the AutoTranslator folder
	- If used ReiPatcher: `<GameDir>/AutoTranslator`
	- If used BepinEx: `<GameDir>/BepinEx/config`
2. Update your config with any API keys, custom urls, glossaries and system prompts. Here is example of ConfigLLM.yaml

```
## LLM configuration
apiKey: "Change me or leave it blank if local endpoint"
url: "api.deepseek.com"
model: "deepseek-chat" # Can leave it blank if using KoboldCpp or Text Generation WebUI. I didn't test with Ollama 
maxConcurrency: 5 # Set how many translation request at the same time. Recommended to leave it at 5 or lower for local. If you get error in console, try lowering it. Minimum must be 1.
modelParams:
  temperature: 0.2
  top_p: 0.9
  frequency_penalty: 0
  presence_penalty: 0
systemPrompt: |
  Translate Simplified Chinese into English, preserving context and meaning. Keep special characters, including escaped sequences (e.g., \n, \=, <), as they appear. Use direct English for professional titles (Envoy, Doctor, Captain) and traditional English for noble/martial titles (Lord, Master, Hero) and Wuxia terms (Qi, sects, cultivation). Use Pinyin for names only, not titles. Default to gender-neutral terms unless gender is explicit. Refine phrasing for readability while maintaining intent and capitalization. Output only the translation.
glossaryPrompt: |
  #Glossary for Consistent Translations
  Use the translation for exact matches.
  ## Terms
glossaryLines:
  - raw: ゲーム
    result: Game
  - raw: 終了
    result: Exit
  - raw: 主人
    result: Master
```

3. Finally update your AutoTranslator INI file with new Endpoint.
	- ```
	  [Service]
	  Endpoint=LLMEndpoint
	  FallbackEndpoint=
	  ```

## Global API Key

We also use global environment variables so you can just set your API Key once and never have to think about it again.

1. Google how to set environment variables on your operating system
2. Set the following environment variable: `AutoTranslator_API_Key` to the value of your API Key.

## Configuration Override Files

~~We have seperate files that can be override any config you have loaded in your config file. This makes it easier to publish game specific prompts.~~

~~These files are:~~
  	~~- `ConfigLLMOverride.yaml`~~
	~~- Use this file to update your system prompt~~
I plan to make PromptLLMEndpoint.yaml too because maybe some specific game can translate well if it has system or glossary prompt with its context, and you can swap with it easily

# Glossary

The glossary feature scans for text that matches entries in the glossary and allows you to instruct how the LLM will translate that word/term/sentence. This reduces hallucinations and mistranslations significantly. The format for a glossary is as follows:

```yaml
- raw: 舅舅
  result: Uncle
```

This is the minimum required for an entry in a glossary. You can also specifically give a seperate glossary prompt to guide your LLM better.

The glossary format supports more options that are mostly there to help translation teams produce more consistent Autotranslator glossaries. The full list is as follows:

```yaml
- raw: 舅舅
  result: Uncle
  transliteration: Jiu Jiu
  context: Endearing way to address an uncle
  checkForHallucination: true
  checkForMistranslation: true
```

Please note `transliteration`, `context` do nothing in the plugin.
Currently `checkForHallucination` and `checkForMistranslation` have not been implemented - stay tuned.

# Fine tuning your prompt

Please note the prompt is what actually tells ChatGPT what to translate. Some things that will help:
- Update the languages eg. Simplified Chinese to English, Japanese to English
- Ensure you add context to the prompt for the game such as 'Wuxia', 'Sengoku Jidai', 'Xanxia', 'Eroge'. 
- Make sure you tell it how to translate names whether you want literal translation or keep the original names

A test project is included with the project. The [PromptTests](./XUnity.AutoTranslator.LlmTranslators.Tests/PromptTests.cs) will let you easily change your prompt based on your model and compare outputs to some ChatGPT4o pretranslated values. These are a good baseline to compare your prompts or other models to, most cases will show you where the model will lose the plot and hallucinate.

[# Download](https://github.com/gickass/XUnity.LLMEndpoint/releases/tag/Release)

# Packages

The assemblies included are the Dev versions of XUnity.AutoTranslator. Feel free to star/fork this repo however you like.
