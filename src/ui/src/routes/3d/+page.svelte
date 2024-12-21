<script lang="ts">
    import { Canvas } from '@threlte/core';
    import Scene from '$lib/3d/Scene.svelte';
</script>

<div class="w-full h-full">
    <Canvas>
        <Scene />
    </Canvas>
</div>

<style>
    div {
        background-color: rgb(30, 41, 59);
    }
</style>