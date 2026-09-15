<script lang="ts">
	import * as api from '$lib/index';
	import { onMount } from 'svelte';
	import { format } from 'date-fns';
	import { configStore } from '$lib/stores/config.store';
	import { clientIdentifierStore } from '$lib/stores/persist.store';
	import Icon from './icon.svelte';
	import { mdiMapMarker } from '@mdi/js';

	api.init();

	function isAllDayEvent(startTime: string, endTime: string) {
		const startDate = new Date(startTime);
		const endDate = new Date(endTime);

		return (
			endDate > startDate &&
			startDate.getHours() === 0 &&
			startDate.getMinutes() === 0 &&
			startDate.getSeconds() === 0 &&
			startDate.getMilliseconds() === 0 &&
			endDate.getHours() === 0 &&
			endDate.getMinutes() === 0 &&
			endDate.getSeconds() === 0 &&
			endDate.getMilliseconds() === 0
		);
	}

	function inclusiveEndDate(endTime: string) {
		// iCalendar represents all-day event end dates exclusively (at the next midnight).
		return new Date(new Date(endTime).getTime() - 1);
	}

	function formatAllDayDates(startTime: string, endTime: string) {
		const startDate = new Date(startTime);
		const endDate = inclusiveEndDate(endTime);
		const startLabel = format(startDate, 'EEEE MMM dd');

		return format(startDate, 'yyyy-MM-dd') === format(endDate, 'yyyy-MM-dd')
			? startLabel
			: `${startLabel} to ${format(endDate, 'EEEE MMM dd')}`;
	}

	let appointments: api.IAppointment[] = $state() as api.IAppointment[];

	onMount(() => {
		GetAppointments();
		const appointmentInterval = setInterval(() => GetAppointments(), 10 * 60 * 1000); //every 10 minutes

		return () => {
			clearInterval(appointmentInterval);
		};
	});

	async function GetAppointments() {
		let appointmentRequest = await api.getAppointments({
			clientIdentifier: $clientIdentifierStore
		});
		if (appointmentRequest.status == 200) {
			appointments = appointmentRequest.data;

			appointments = appointmentRequest.data.sort((a, b) => {
				return new Date(a.startTime ?? '').getTime() - new Date(b.startTime ?? '').getTime();
			});
		}
	}
</script>

{#if appointments}
	<div
		id="appointments"
		class="fixed top-0 right-0 w-auto z-10 text-center text-primary mr-1 mt-5 max-w-[20%] hidden lg:block md:min-w-[10%]
		{$configStore.style == 'transition' ? 'bg-gradient-to-r from-secondary from-0% pr-10' : ''}
		{$configStore.style == 'solid' ? 'bg-secondary rounded-tl-2xl' : ''}
		{$configStore.style == 'blur' ? 'backdrop-blur-lg rounded-tl-2xl' : ''}"
	>
		<!-- <div class="text-4xl mx-8 font-bold">Appointments</div> -->
		<div class="">
			{#each appointments as appointment}
				<div class="bg-opacity-90 mb-2 text-left rounded-md p-3">
					{#if isAllDayEvent(appointment.startTime ?? '', appointment.endTime ?? '')}
						<p class="text-s">
							<b><u>{formatAllDayDates(appointment.startTime ?? '', appointment.endTime ?? '')}</u></b>
						</p>
						<p class="text-xs">All day</p>
					{:else if format(new Date(appointment.startTime ?? ''), 'MMMM dd yyyy') === format(new Date(appointment.endTime ?? ''), 'MMMM dd yyyy')}
						<p class="text-s">
							<b><u>{format(new Date(appointment.startTime ?? ''), 'EEEE MMM dd')}</u></b>
						</p>
						<p class="text-xs">
							{format(appointment.startTime ?? '', 'hh:mm b')} - {format(
								appointment.endTime ?? '',
								'hh:mm b'
							)}
						</p>
					{:else}
						<p class="text-s">
							<b
								><u
									>{format(new Date(appointment.startTime ?? ''), 'EEEE MMM dd')} to {format(
										new Date(appointment.endTime ?? ''),
										'EEEE MMM dd'
									)}</u
								></b
							>
						</p>
					{/if}

					{appointment.summary}
					{#if appointment.location}
						<p class="text-xs font-light">
							<!-- <Icon path={mdiMapMarker}/> -->
							<i>{appointment.location}</i>
						</p>
					{/if}
					<!--
					{#if appointment.description}
						<br />
						<p class="text-xs font-light">{appointment.description}</p>
					{/if}
					-->
				</div>
			{/each}
		</div>
	</div>
{/if}
