<script lang="ts">
	import * as api from '$lib/index';
	import { onMount } from 'svelte';
	import { format } from 'date-fns';
	import * as locale from 'date-fns/locale';
	import { configStore } from '$lib/stores/config.store';
	import { clientIdentifierStore } from '$lib/stores/persist.store';
	import * as iconsImport from '$lib/assets/icons';
	const icons: Record<string, string> = iconsImport as Record<string, string>;

    api.init();

	let weather = $state<api.IWeather | null>(null);
    let now = $state(new Date());

	onMount(() => {
		const interval = setInterval(() => {
			now = new Date();
		}, 1000);

		getWeather();
		const weatherInterval = setInterval(() => getWeather(), 10 * 60 * 1000);

		return () => {
			clearInterval(interval);
			clearInterval(weatherInterval);
		};
	});
    
    async function getWeather() {
		try {
			const weatherRequest = await api.getWeather({ clientIdentifier: $clientIdentifierStore });
			if (weatherRequest.status === 200) {
				weather = weatherRequest.data;
			} else {
				console.warn('Unexpected weather status:', weatherRequest.status);
			}
		} catch (err) {
			console.error('Error fetching weather:', err);
		}
	}

</script>

<div
	id="weather"
	class="fixed top-0 left-0 z-10 text-center text-primary
	{$configStore.style == 'solid' ? 'bg-secondary rounded-tr-2xl' : ''}
	{$configStore.style == 'transition' ? 'bg-gradient-to-r from-secondary from-0% pr-10' : ''}
	{$configStore.style == 'blur' ? 'backdrop-blur-lg rounded-tr-2xl' : ''}	
	drop-shadow-2xl p-3"
>
	{#if weather}
        <div
            id="weatherinfo"
            class="text-xl sm:text-xl md:text-2xl lg:text-3xl font-semibold text-shadow-sm weather-info"
        >
			{#if weather.iconId}
				<img src="{icons[weather.iconId.replace('-','')]}" class="icon-weather" alt="{weather.description}">
			{/if}
         
            <div class="weather-temperature">{weather.temperature?.toFixed(1)}</div>
            <div class="weather-unit">{weather.unit}</div>
        </div>
        <div id = "weatherHighLow" class="text-sm sm:text-sm md:text-md lg:text-xl text-shadow-sm">
            <div class="weather-hilow">H {weather.tempHigh?.toFixed(0)}° / L {weather.tempLow?.toFixed(0)}°</div>
            <div class="weather-precip">Precip: {weather.precip}%</div>
        </div>

        {#if $configStore.showWeatherDescription}
            <p id="weatherdesc" class="text-sm sm:text-sm md:text-md lg:text-xl text-shadow-sm">
                {weather.description}
            </p>
        {/if}

        {#each weather.hourlyForecast ?? [] as forecast, index}
			<div class="forecast-item">
				<p>{forecast.time != null ? new Date(Number(forecast.time) * 1000).toLocaleTimeString(undefined, { hour: 'numeric', hour12: true }) : ''}
				{forecast.temperature !== undefined ? Math.round(forecast.temperature) : '--'}°
				Precip: {forecast.precipProbability}%</p>
			</div>
		{/each}

		{#each weather.dailyForecast ?? [] as forecast, index}
			<div class="forecast-item">
				<p>{forecast.time != null ? new Date(Number(forecast.time) * 1000).toLocaleDateString(undefined, { weekday: 'short' }) : ''}
					H {forecast.temperatureHigh !== undefined ? Math.round(forecast.temperatureHigh) : '--'}°
					/ L {forecast.temperatureLow !== undefined ? Math.round(forecast.temperatureLow) : '--'}
				Precip: {forecast.precipProbability}%</p>
			</div>
		{/each}


	{/if}
</div>