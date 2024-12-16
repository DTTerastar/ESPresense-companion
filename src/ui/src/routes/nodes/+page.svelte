<script lang="ts">
	import NodesTable from '$lib/NodesTable.svelte';
	import { getToastStore, SlideToggle } from '@skeletonlabs/skeleton';
	import type { ToastSettings } from '@skeletonlabs/skeleton';
	import { base } from '$app/paths';

	let autoUpdate = false;
	let beta = false;
	const toastStore = getToastStore();

	function saveSettings() {
		fetch(`${base}/api/state/global/updating`, {
			method: 'PUT',
			headers: {
				'Content-Type': 'application/json'
			},
			body: JSON.stringify({ auto_update: autoUpdate, beta: beta })
		})
		.then((response) => {
			if (response.status != 200) throw new Error(response.statusText);
		})
		.catch((e) => {
			console.log(e);
			const t: ToastSettings = { message: e, background: 'variant-filled-error' };
			toastStore.trigger(t);
		});
	}

	// Load initial settings
	fetch(`${base}/api/state/global/updating`)
		.then((response) => response.json())
		.then((data) => {
			autoUpdate = data.auto_update;
			beta = data.beta;
		})
		.catch((e) => {
			console.log(e);
			const t: ToastSettings = { message: e, background: 'variant-filled-error' };
			toastStore.trigger(t);
		});
</script>

<svelte:head>
	<title>ESPresense Companion: Nodes</title>
</svelte:head>

<div class="container mx-auto p-2">
	<div class="flex justify-between items-center my-2 px-2">
		<h1 class="text-3xl font-bold">Nodes</h1>
		<div class="flex items-center space-x-4">
			<div class="flex items-center space-x-4">
				<SlideToggle
					name="auto-update"
					bind:checked={autoUpdate}
					on:change={saveSettings}
				>Auto Update</SlideToggle>
				<SlideToggle
					name="beta"
					bind:checked={beta}
					on:change={saveSettings}
				>Beta</SlideToggle>
			</div>
		</div>
	</div>

	<NodesTable />
</div>
